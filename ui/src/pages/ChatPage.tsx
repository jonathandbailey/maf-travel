import Exchanges from "../features/chat/components/Exchanges";
import ChatInput from "../features/chat/components/ChatInput";
import TravelPlan from "../features/travel/components/TravelPlan";
import Welcome from "../features/travel/components/Welcome";
import { useState } from "react";
import { Button } from "antd";
import { useChatAgent } from "../features/chat/hooks/useChatAgent";
import { useTravelPlan } from "../features/travel/hooks/useTravelPlan";
import { useTravelPlanStore } from "../features/travel/store/travelPlanStore";
import "./ChatPage.css";

const ChatPage = () => {
    const [inputValue, setInputValue] = useState("");
    const { exchanges, isStreaming, sendMessage, handleCancel, client } = useChatAgent();
    const createPlan = useTravelPlanStore((s) => s.createPlan);
    useTravelPlan(client);

    const submitMessage = () => {
        const text = inputValue;
        setInputValue("");
        sendMessage(text);
    };

    const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter") submitMessage();
    };

    const handleSubmit = () => submitMessage();

    return (
        <div className="chat-page">
            <div className="chat-main">
                <div className="chat-messages">
                    {exchanges.length === 0 ? (
                        <Welcome />
                    ) : (
                        <Exchanges exchanges={exchanges} />
                    )}
                </div>
                <ChatInput
                    value={inputValue}
                    onChange={setInputValue}
                    onKeyDown={handleKeyDown}
                    onSuggestionSelect={(suggestion) => sendMessage(suggestion)}
                    onSubmit={handleSubmit}
                    isStreaming={isStreaming}
                    onCancel={handleCancel}
                />
            </div>
            <div className="chat-sidebar">
                <TravelPlan />
            </div>
        </div>
    )
}

export default ChatPage;
