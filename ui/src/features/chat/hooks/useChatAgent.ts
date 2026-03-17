import { useState, useEffect } from "react";
import { EventType, randomUUID, type BaseEvent, type StateSnapshotEvent, type TextMessageContentEvent } from "@ag-ui/client";
import type { StatusUpdate } from "../domain/StatusUpdate";
import { ChatAgentClient } from "../services/ChatAgentClient";
import { useTravelPlanStore } from "../../travel/store/travelPlanStore";
import { useSessionStore } from "@/app/store/sessionStore";

export interface ExchangeItem {
    id: string;
    userContent: string;
    assistantContent?: string;
    statusUpdates: StatusUpdate[];
    error?: string;
}

interface StatusUpdatePayload {
    Type: string;
    Source: string;
    Status: string;
    Details: string;
}

interface TypedSnapshot {
    Type: string;
    Payload?: StatusUpdatePayload;
}

function isTypedSnapshot(value: unknown): value is TypedSnapshot {
    return typeof value === 'object' && value !== null && 'Type' in value;
}

const AGENT_URL = `${import.meta.env.VITE_API_BASE_URL}/ag-ui`;

export function useChatAgent() {
    const [exchanges, setExchanges] = useState<ExchangeItem[]>([]);
    const [isStreaming, setIsStreaming] = useState(false);

    const [client] = useState(() => new ChatAgentClient(AGENT_URL, useSessionStore.getState().sessionId ?? randomUUID(), {
        onRunStarted: (exchangeId, userText) => {
            setExchanges((prev) => [...prev, { id: exchangeId, userContent: userText, statusUpdates: [] }]);
            setIsStreaming(true);
        },

        onEvent: (event: BaseEvent, exchangeId: string) => {
            if (event.type === EventType.TEXT_MESSAGE_CONTENT) {
                const delta = (event as TextMessageContentEvent).delta || '';
                setExchanges((prev) =>
                    prev.map((ex) => ex.id === exchangeId
                        ? { ...ex, assistantContent: (ex.assistantContent ?? '') + delta }
                        : ex
                    )
                );
            }
            if (event.type === EventType.STATE_SNAPSHOT) {
                const snapshot = (event as StateSnapshotEvent).snapshot;
                if (isTypedSnapshot(snapshot) && snapshot.Type === 'StatusUpdate' && snapshot.Payload) {
                    const payload = snapshot.Payload;
                    setExchanges((prev) =>
                        prev.map((ex) => ex.id === exchangeId
                            ? { ...ex, statusUpdates: [...ex.statusUpdates, {
                                type: payload.Type,
                                source: payload.Source,
                                status: payload.Status,
                                details: payload.Details,
                            }] }
                            : ex
                        )
                    );
                }
            }
        },

        onRunFailed: (exchangeId, error) => {
            console.error("Agent run failed:", error);
            setExchanges((prev) =>
                prev.map((ex) => ex.id === exchangeId ? { ...ex, error: error.message } : ex)
            );
        },

        onRunCompleted: () => {
            setIsStreaming(false);
        },
    }));

    useEffect(() => {
        return useSessionStore.subscribe((state, prevState) => {
            if (state.sessionId && state.sessionId !== prevState.sessionId) {
                client.setThreadId(state.sessionId);
            }
        });
    }, [client]);

    useEffect(() => {
        return useTravelPlanStore.subscribe((state, prevState) => {
            if (state.planVersion !== prevState.planVersion) {
                setExchanges([]);
            }
        });
    }, []);

    const sendMessage = (text: string) => {
        if (!text.trim()) return;
        client.sendMessage(text);
    };

    const handleCancel = () => {
        client.cancel();
    };

    return { exchanges, isStreaming, sendMessage, handleCancel, client };
}
