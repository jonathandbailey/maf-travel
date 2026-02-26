import { Card, Input } from "antd";
import Exchange from "../features/chat/Exchange";
import { useState } from "react";

interface ExchangeItem {
    id: string;
    userContent: string;
    assistantContent?: string;
    error?: string;
}

const ChatPage = () => {
    const [exchanges, setExchanges] = useState<ExchangeItem[]>([]);
    const [inputValue, setInputValue] = useState("");

    const handleKeyDown = async (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key !== "Enter") return;
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
                    marginBottom: 0,
                    marginTop: 0,
                    boxShadow: "0 4px 8px rgba(0, 0, 0, 0.1)",
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