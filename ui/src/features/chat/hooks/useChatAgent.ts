import { useState, useRef } from "react";
import { EventType, randomUUID, type BaseEvent, type StateSnapshotEvent } from "@ag-ui/client";
import type { StatusUpdate } from "../domain/StatusUpdate";
import { ChatAgentClient, type ChatAgentCallbacks } from "../services/ChatAgentClient";

export interface ExchangeItem {
    id: string;
    userContent: string;
    assistantContent?: string;
    statusUpdates: StatusUpdate[];
    error?: string;
}

const AGENT_URL = `${import.meta.env.VITE_API_BASE_URL}/ag-ui`;

export function useChatAgent() {
    const [exchanges, setExchanges] = useState<ExchangeItem[]>([]);
    const [isStreaming, setIsStreaming] = useState(false);

    const callbacksRef = useRef<ChatAgentCallbacks>({
        onRunStarted: () => {},
        onEvent: () => {},
        onRunFailed: () => {},
        onRunCompleted: () => {},
    });

    const handleTextContent = (event: BaseEvent, exchangeId: string) => {
        const delta = (event as any).delta || '';
        setExchanges((prev) =>
            prev.map((ex) => ex.id === exchangeId
                ? { ...ex, assistantContent: (ex.assistantContent ?? '') + delta }
                : ex
            )
        );
    };

    const handleStatusUpdate = (payload: any, exchangeId: string) => {
        const statusUpdate: StatusUpdate = {
            type: payload.Type,
            source: payload.Source,
            status: payload.Status,
            details: payload.Details,
        };
        setExchanges((prev) =>
            prev.map((ex) => ex.id === exchangeId
                ? { ...ex, statusUpdates: [...ex.statusUpdates, statusUpdate] }
                : ex
            )
        );
    };

    const handleStateSnapshot = (event: BaseEvent, exchangeId: string) => {
        const snapshot = (event as StateSnapshotEvent).snapshot;
        if (typeof snapshot !== 'object' || snapshot === null || !('Type' in snapshot)) return;

        if (snapshot.Type === 'StatusUpdate' && (snapshot as any).Payload)
            handleStatusUpdate((snapshot as any).Payload, exchangeId);
    };

    callbacksRef.current = {
        onRunStarted: (exchangeId, userText) => {
            setExchanges((prev) => [...prev, { id: exchangeId, userContent: userText, statusUpdates: [] }]);
            setIsStreaming(true);
        },

        onEvent: (event: BaseEvent, exchangeId: string) => {
            if (event.type === EventType.TEXT_MESSAGE_CONTENT) handleTextContent(event, exchangeId);
            if (event.type === EventType.STATE_SNAPSHOT)       handleStateSnapshot(event, exchangeId);
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
    };

    const clientRef = useRef<ChatAgentClient | null>(null);
    if (!clientRef.current) {
        clientRef.current = new ChatAgentClient(AGENT_URL, randomUUID(), {
            onRunStarted: (...args) => callbacksRef.current.onRunStarted(...args),
            onEvent: (...args) => callbacksRef.current.onEvent(...args),
            onRunFailed: (...args) => callbacksRef.current.onRunFailed(...args),
            onRunCompleted: (...args) => callbacksRef.current.onRunCompleted(...args),
        });
    }

    const sendMessage = (text: string) => {
        if (!text.trim()) return;
        clientRef.current!.sendMessage(text);
    };

    const handleCancel = () => {
        clientRef.current!.cancel();
    };

    const handleNewPlan = () => {
        setExchanges([]);
        clientRef.current!.setThreadId(randomUUID());
    };

    return { exchanges, isStreaming, sendMessage, handleCancel, handleNewPlan, client: clientRef.current };
}
