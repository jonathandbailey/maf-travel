import { useState, useEffect } from "react";
import { EventType, randomUUID, type BaseEvent, type StateSnapshotEvent, type TextMessageContentEvent } from "@ag-ui/client";
import type { StatusUpdate } from "../domain/StatusUpdate";
import { ChatAgentClient } from "../services/ChatAgentClient";
import { useTravelPlanStore } from "../../travel/store/travelPlanStore";
import { useSessionStore } from "@/app/store/sessionStore";
import { useChatStore } from "../store/chatStore";

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
    const [streamingExchange, setStreamingExchange] = useState<ExchangeItem | null>(null);
    const [isStreaming, setIsStreaming] = useState(false);

    const addExchange = useChatStore((s) => s.addExchange);
    const clearExchanges = useChatStore((s) => s.clearExchanges);

    const [client] = useState(() => new ChatAgentClient(AGENT_URL, useSessionStore.getState().sessionId ?? randomUUID(), {
        onRunStarted: (exchangeId, userText) => {
            setStreamingExchange({ id: exchangeId, userContent: userText, statusUpdates: [] });
            setIsStreaming(true);
        },

        onEvent: (event: BaseEvent, exchangeId: string) => {
            if (event.type === EventType.TEXT_MESSAGE_CONTENT) {
                const delta = (event as TextMessageContentEvent).delta || '';
                setStreamingExchange((prev) =>
                    prev?.id === exchangeId
                        ? { ...prev, assistantContent: (prev.assistantContent ?? '') + delta }
                        : prev
                );
            }
            if (event.type === EventType.STATE_SNAPSHOT) {
                const snapshot = (event as StateSnapshotEvent).snapshot;
                if (isTypedSnapshot(snapshot) && snapshot.Type === 'StatusUpdate' && snapshot.Payload) {
                    const payload = snapshot.Payload;
                    setStreamingExchange((prev) =>
                        prev?.id === exchangeId
                            ? { ...prev, statusUpdates: [...prev.statusUpdates, {
                                type: payload.Type,
                                source: payload.Source,
                                status: payload.Status,
                                details: payload.Details,
                            }] }
                            : prev
                    );
                }
            }
        },

        onRunFailed: (exchangeId, error) => {
            console.error("Agent run failed:", error);
            setStreamingExchange((prev) => {
                if (prev?.id === exchangeId) {
                    const failed = { ...prev, error: error.message };
                    useChatStore.getState().addExchange(failed);
                    return null;
                }
                return prev;
            });
            setIsStreaming(false);
        },

        onRunCompleted: () => {
            setStreamingExchange((prev) => {
                if (prev) {
                    useChatStore.getState().addExchange(prev);
                }
                return null;
            });
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
                clearExchanges();
                setStreamingExchange(null);
            }
        });
    }, [clearExchanges]);

    const sendMessage = (text: string) => {
        if (!text.trim()) return;
        client.sendMessage(text);
    };

    const handleCancel = () => {
        client.cancel();
    };

    return { streamingExchange, isStreaming, sendMessage, handleCancel, client };
}
