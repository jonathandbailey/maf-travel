import Exchange from "../features/chat/Exchange";
import ChatInput from "../features/chat/components/ChatInput";
import { useState } from "react";
import { EventType, HttpAgent, randomUUID, type BaseEvent, type StateSnapshotEvent } from "@ag-ui/client";
import type { StatusUpdate } from "../features/chat/domain/StatusUpdate";

interface ExchangeItem {
    id: string;
    userContent: string;
    assistantContent?: string;
    statusUpdates: StatusUpdate[];
    error?: string;
}

const AGENT_URL = `${import.meta.env.VITE_API_BASE_URL}/ag-ui`;


const ChatPage = () => {
    const [exchanges, setExchanges] = useState<ExchangeItem[]>([]);
    const [inputValue, setInputValue] = useState("");
    const [threadId] = useState(randomUUID());

    const handleKeyDown = async (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key !== "Enter") return;

        const text = inputValue;
        if (!text.trim()) return;

        setInputValue("");

        const exchangeId = randomUUID();
        setExchanges((prev) => [...prev, { id: exchangeId, userContent: text, statusUpdates: [] }]);

        const agent = new HttpAgent({
            url: AGENT_URL,
            threadId: threadId,
            initialMessages: [{ id: randomUUID(), role: "user", content: text }],
        });

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
                        // Handle StatusUpdate type
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
                        console.log("Received agent event:", event);
                    }
                }
            }
        });

        await agent.runAgent({ runId: randomUUID() });
    }

    return (<>

        <div style={{ display: "flex", flexDirection: "column", height: "100%", overflow: "hidden" }}>
            <div style={{ flex: 1, overflow: "auto", width: "100%" }}>
                <div style={{ maxWidth: 768, margin: "0 auto" }}>
                    {exchanges.map((ex) => (
                        <Exchange key={ex.id} userContent={ex.userContent} assistantContent={ex.assistantContent} statusUpdates={ex.statusUpdates} error={ex.error} />
                    ))}
                </div>
            </div>
            <ChatInput
                value={inputValue}
                onChange={setInputValue}
                onKeyDown={handleKeyDown}
            />

        </div>
    </>)
}

export default ChatPage;