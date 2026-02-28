import { HttpAgent, randomUUID, type BaseEvent } from "@ag-ui/client";

export interface ChatAgentCallbacks {
    onRunStarted: (exchangeId: string, userText: string) => void;
    onEvent: (event: BaseEvent, exchangeId: string) => void;
    onRunFailed: (exchangeId: string, error: Error) => void;
    onRunCompleted: (exchangeId: string) => void;
}

export class ChatAgentClient {
    private readonly url: string;
    private readonly callbacks: ChatAgentCallbacks;
    private agent: InstanceType<typeof HttpAgent> | null = null;
    private threadId: string;

    constructor(url: string, threadId: string, callbacks: ChatAgentCallbacks) {
        this.url = url;
        this.threadId = threadId;
        this.callbacks = callbacks;
    }

    setThreadId(id: string): void {
        this.agent?.abortRun();
        this.agent = null;
        this.threadId = id;
    }

    async sendMessage(text: string): Promise<void> {
        const exchangeId = randomUUID();
        this.callbacks.onRunStarted(exchangeId, text);

        const agent = new HttpAgent({
            url: this.url,
            threadId: this.threadId,
            initialMessages: [{ id: randomUUID(), role: "user", content: text }],
        });
        this.agent = agent;

        agent.subscribe({
            onRunFailed: ({ error }) => this.callbacks.onRunFailed(exchangeId, error),
            onEvent: ({ event }: { event: BaseEvent }) => this.callbacks.onEvent(event, exchangeId),
        });

        try {
            await agent.runAgent({ runId: randomUUID() });
        } finally {
            this.callbacks.onRunCompleted(exchangeId);
            this.agent = null;
        }
    }

    cancel(): void {
        this.agent?.abortRun();
    }
}
