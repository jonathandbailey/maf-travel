import { Card, Input } from "antd";
import Exchange from "../features/chat/Exchange";
import { useState } from "react";
import { HttpAgent, randomUUID } from "@ag-ui/client";

interface ExchangeItem {
    id: string;
    userContent: string;
    assistantContent?: string;
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
        setExchanges((prev) => [...prev, { id: exchangeId, userContent: text }]);

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
        });

        const runResult = await agent.runAgent({ runId: randomUUID() });

        if (runResult.newMessages?.length) {
            const assistantContent = runResult.newMessages.map((m) => m.content).join("\n");
            setExchanges((prev) =>
                prev.map((ex) => ex.id === exchangeId ? { ...ex, assistantContent } : ex)
            );
        }
    }

    return (<>

        <div style={{ display: "flex", flexDirection: "column", height: "100%", overflow: "hidden", alignItems: "center" }}>
            <div style={{ flex: 1, overflow: "auto", width: "100%", maxWidth: 768 }}>
                {exchanges.map((ex) => (
                    <Exchange key={ex.id} userContent={ex.userContent} assistantContent={ex.assistantContent} error={ex.error} />
                ))}
            </div>
            <Card
                style={{

                    width: "100%",
                    maxWidth: 768,
                    minWidth: 700,
                    marginBottom: 24,
                    marginTop: 0,
                    boxShadow: "0 0 8px rgba(0, 0, 0, 0.1)",
                    borderRadius: 16,
                }}
            >
                <Input
                    placeholder="Ask me anything..."
                    variant="borderless"
                    value={inputValue}
                    onChange={(e) => setInputValue(e.target.value)}
                    onKeyDown={handleKeyDown}
                    style={{ flex: 1, width: "100%" }}
                />
            </Card>

        </div>
    </>)
}

export default ChatPage;