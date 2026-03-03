import { EventType, HttpAgent, randomUUID, type BaseEvent, type StateSnapshotEvent } from "@ag-ui/client";

export interface ChatAgentCallbacks {
    onRunStarted: (exchangeId: string, userText: string) => void;
    onEvent: (event: BaseEvent, exchangeId: string) => void;
    onRunFailed: (exchangeId: string, error: Error) => void;
    onRunCompleted: (exchangeId: string) => void;
}

export type EventHandler = (event: BaseEvent, exchangeId: string) => void;

export class ChatAgentClient {
    private readonly url: string;
    private readonly callbacks: ChatAgentCallbacks;
    private agent: InstanceType<typeof HttpAgent> | null = null;
    private threadId: string;
    private readonly eventHandlers = new Set<EventHandler>();
    private currentExchangeId: string | null = null;

    constructor(url: string, threadId: string, callbacks: ChatAgentCallbacks) {
        this.url = url;
        this.threadId = threadId;
        this.callbacks = callbacks;
    }

    addEventHandler(handler: EventHandler): void {
        this.eventHandlers.add(handler);
    }

    removeEventHandler(handler: EventHandler): void {
        this.eventHandlers.delete(handler);
    }

    setThreadId(id: string): void {
        this.agent?.abortRun();
        this.agent = null;
        this.threadId = id;
    }

    async sendMessage(text: string): Promise<void> {
        const exchangeId = randomUUID();
        this.currentExchangeId = exchangeId;
        this.callbacks.onRunStarted(exchangeId, text);

        const agent = new HttpAgent({
            url: this.url,
            threadId: this.threadId,
            initialMessages: [{ id: randomUUID(), role: "user", content: text }],
        });
        this.agent = agent;

        agent.subscribe({
            onRunFailed: ({ error }) => this.callbacks.onRunFailed(exchangeId, error),
            onEvent: ({ event }: { event: BaseEvent }) => {
                this.callbacks.onEvent(event, exchangeId);
                this.eventHandlers.forEach((h) => h(event, exchangeId));
                if (event.type === EventType.STATE_SNAPSHOT) {
                    const snapshot = (event as StateSnapshotEvent).snapshot as { Type?: string; Payload?: { Status?: string } };
                    if (snapshot?.Type === "RunError") {
                        this.callbacks.onRunFailed(exchangeId, new Error(snapshot.Payload?.Status ?? "An error occurred"));
                    }
                }
            },
        });

        try {
            await agent.runAgent({ runId: randomUUID() });
        } finally {
            this.currentExchangeId = null;
            this.callbacks.onRunCompleted(exchangeId);
            this.agent = null;
        }
    }

    cancel(): void {
        if (this.currentExchangeId) {
            this.callbacks.onRunFailed(this.currentExchangeId, new Error("Your request was cancelled."));
        }
        this.agent?.abortRun();
    }
}
