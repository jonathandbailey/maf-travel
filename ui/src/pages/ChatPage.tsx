import Exchange from "../features/chat/Exchange";
import ChatInput from "../features/chat/components/ChatInput";
import TravelPlan from "../features/travel/components/TravelPlan";
import Welcome from "../features/travel/components/Welcome";
import { useState } from "react";
import { Button } from "antd";
import { EventType, HttpAgent, randomUUID, type BaseEvent, type StateSnapshotEvent } from "@ag-ui/client";
import type { StatusUpdate } from "../features/chat/domain/StatusUpdate";
import type { TravelPlan as TravelPlanModel } from "../features/travel/domain/TravelPlan";

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
    const [threadId, setThreadId] = useState(randomUUID());
    const [isStreaming, setIsStreaming] = useState(false);

    const handleNewPlan = () => {
        setExchanges([]);
        setTravelPlan({ origin: null, destination: null, startDate: null, endDate: null, numberOfTravelers: null });
        setThreadId(randomUUID());
    };
    const [travelPlan, setTravelPlan] = useState<TravelPlanModel>({
        origin: null,
        destination: null,
        startDate: null,
        endDate: null,
        numberOfTravelers: null,
    });

    const sendMessage = async (text: string) => {
        if (!text.trim()) return;

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
                        const payload = (snapshot as any).Payload;
                        if (payload) {
                            setTravelPlan({
                                origin: payload.Origin ?? null,
                                destination: payload.Destination ?? null,
                                startDate: payload.DepartureDate ?? null,
                                endDate: payload.ReturnDate ?? null,
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
        }
    }

    return (
        <div style={{ display: "flex", flexDirection: "row", height: "100%", overflow: "hidden" }}>
            <div style={{ display: "flex", flexDirection: "column", flex: 1, overflow: "hidden" }}>
                <div style={{ flex: 1, overflow: "auto", width: "100%" }}>
                    {exchanges.length === 0 ? (
                        <Welcome />
                    ) : (
                        <div style={{ maxWidth: 768, margin: "0 auto" }}>
                            {exchanges.map((ex) => (
                                <Exchange key={ex.id} userContent={ex.userContent} assistantContent={ex.assistantContent} statusUpdates={ex.statusUpdates} error={ex.error} />
                            ))}
                        </div>
                    )}
                </div>
                <ChatInput
                    value={inputValue}
                    onChange={setInputValue}
                    onKeyDown={(e) => {
                        if (e.key !== "Enter") return;
                        const text = inputValue;
                        setInputValue("");
                        sendMessage(text);
                    }}
                    onSuggestionSelect={(suggestion) => sendMessage(suggestion)}
                    onSubmit={() => {
                        const text = inputValue;
                        setInputValue("");
                        sendMessage(text);
                    }}
                    isStreaming={isStreaming}
                />
            </div>
            <div style={{ width: 320, padding: 16, alignSelf: "flex-start", display: "flex", flexDirection: "column", gap: 12 }}>
                <Button type="primary" block onClick={handleNewPlan}>New Travel Plan</Button>
                <TravelPlan travelPlan={travelPlan} />
            </div>
        </div>
    )
}

export default ChatPage;