import { useState, useRef } from "react";
import { EventType, HttpAgent, randomUUID, type BaseEvent, type StateSnapshotEvent } from "@ag-ui/client";
import type { StatusUpdate } from "../domain/StatusUpdate";
import type { TravelPlan } from "../../travel/domain/TravelPlan";

interface ExchangeItem {
    id: string;
    userContent: string;
    assistantContent?: string;
    statusUpdates: StatusUpdate[];
    error?: string;
}

const AGENT_URL = `${import.meta.env.VITE_API_BASE_URL}/ag-ui`;

export function useChatAgent() {
    const [exchanges, setExchanges] = useState<ExchangeItem[]>([]);
    const [threadId, setThreadId] = useState(randomUUID());
    const [isStreaming, setIsStreaming] = useState(false);
    const [travelPlan, setTravelPlan] = useState<TravelPlan>({
        origin: null,
        destination: null,
        startDate: null,
        endDate: null,
        numberOfTravelers: null,
    });
    const agentRef = useRef<InstanceType<typeof HttpAgent> | null>(null);

    const handleCancel = () => {
        agentRef.current?.abortRun();
    };

    const handleNewPlan = () => {
        setExchanges([]);
        setTravelPlan({ origin: null, destination: null, startDate: null, endDate: null, numberOfTravelers: null });
        setThreadId(randomUUID());
    };

    const sendMessage = async (text: string) => {
        if (!text.trim()) return;

        const exchangeId = randomUUID();
        setExchanges((prev) => [...prev, { id: exchangeId, userContent: text, statusUpdates: [] }]);

        const agent = new HttpAgent({
            url: AGENT_URL,
            threadId: threadId,
            initialMessages: [{ id: randomUUID(), role: "user", content: text }],
        });
        agentRef.current = agent;

        agent.subscribe({
            onRunFailed: ({ error }) => {
                console.error("Agent run failed:", error);
                setExchanges((prev) =>
                    prev.map((ex) => ex.id === exchangeId ? { ...ex, error: error.message } : ex)
                );
            },
            onEvent: ({ event }: { event: BaseEvent }) => {
                if (event.type === EventType.TEXT_MESSAGE_CONTENT) {
                    const delta = (event as any).delta || '';
                    setExchanges((prev) =>
                        prev.map((ex) => ex.id === exchangeId
                            ? { ...ex, assistantContent: (ex.assistantContent ?? '') + delta }
                            : ex
                        )
                    );
                }

                if (event.type === EventType.STATE_SNAPSHOT) {
                    const snapshotEvent = event as StateSnapshotEvent;
                    const snapshot = snapshotEvent.snapshot;

                    if (typeof snapshot === 'object' && snapshot !== null && 'Type' in snapshot && snapshot.Type === 'StatusUpdate') {
                        const payload = (snapshot as any).Payload;
                        if (payload) {
                            const statusUpdate: StatusUpdate = {
                                type: payload.Type,
                                source: payload.Source,
                                status: payload.Status,
                                details: payload.Details
                            };
                            setExchanges((prev) =>
                                prev.map((ex) => ex.id === exchangeId
                                    ? { ...ex, statusUpdates: [...ex.statusUpdates, statusUpdate] }
                                    : ex
                                )
                            );
                        }
                    }

                    if (typeof snapshot === 'object'
                        && snapshot !== null
                        && 'Type' in snapshot && snapshot.Type === 'TravelPlanUpdate') {
                        const payload = (snapshot as any).Payload;
                        console.log("Received TravelPlanUpdate payload:", payload);
                        if (payload) {
                            setTravelPlan({
                                origin: payload.Origin ?? null,
                                destination: payload.Destination ?? null,
                                startDate: payload.StartDate ?? null,
                                endDate: payload.EndDate ?? null,
                                numberOfTravelers: payload.NumberOfTravelers ?? null,
                            });
                        }
                    }
                }
            }
        });

        try {
            setIsStreaming(true);
            await agent.runAgent({ runId: randomUUID() });
        } finally {
            setIsStreaming(false);
            agentRef.current = null;
        }
    };

    return { exchanges, travelPlan, isStreaming, sendMessage, handleCancel, handleNewPlan };
}
