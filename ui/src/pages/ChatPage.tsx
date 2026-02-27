import Exchange from "../features/chat/components/Exchange";
import ChatInput from "../features/chat/components/ChatInput";
import TravelPlan from "../features/travel/components/TravelPlan";
import Welcome from "../features/travel/components/Welcome";
import { useState } from "react";
import { Button } from "antd";
import { useChatAgent } from "../features/chat/hooks/useChatAgent";

const ChatPage = () => {
    const [inputValue, setInputValue] = useState("");
    const { exchanges, travelPlan, isStreaming, sendMessage, handleCancel, handleNewPlan } = useChatAgent();

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
                    onCancel={handleCancel}
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
