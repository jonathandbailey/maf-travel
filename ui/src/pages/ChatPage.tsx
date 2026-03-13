import { useEffect } from 'react';
import { useParams } from 'react-router-dom';
import Exchanges from "../features/chat/components/Exchanges";
import ChatInput from "../features/chat/components/ChatInput";
import TravelPlan from "../features/travel/components/TravelPlan";
import Welcome from "../features/travel/components/Welcome";
import { useState } from "react";
import { useChatAgent } from "../features/chat/hooks/useChatAgent";
import { useTravelPlan } from "../features/travel/hooks/useTravelPlan";
import { useSessionStore } from '../app/store/sessionStore';
import { useTravelPlanStore } from '../features/travel/store/travelPlanStore';
import { getTravelPlan } from '../features/travel/services/travelPlanService';
import "./ChatPage.css";

const ChatPage = () => {
    const { id } = useParams<{ id: string }>();
    const setSessionId = useSessionStore((s) => s.setSessionId);
    const createPlan = useTravelPlanStore((s) => s.createPlan);
    const updatePlan = useTravelPlanStore((s) => s.updatePlan);

    useEffect(() => {
        createPlan();
        getTravelPlan(id!).then(({ origin, destination, startDate, endDate, numberOfTravelers, sessionId }) => {
            updatePlan({ origin, destination, startDate, endDate, numberOfTravelers });
            if (sessionId) setSessionId(sessionId);
        });
    }, [id, setSessionId, createPlan, updatePlan]);

    const [inputValue, setInputValue] = useState("");
    const { exchanges, isStreaming, sendMessage, handleCancel, client } = useChatAgent();
    useTravelPlan(client);

    const submitMessage = () => {
        if (!inputValue.trim()) return;
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
