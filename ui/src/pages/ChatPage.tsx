import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { MenuFoldOutlined, MessageOutlined } from '@ant-design/icons';
import Exchanges from "@/features/chat/components/Exchanges";
import ChatInput from "@/features/chat/components/ChatInput";
import TravelPlan from "@/features/travel/components/TravelPlan";
import Welcome from "@/features/travel/components/Welcome";
import { useChatAgent } from "@/features/chat/hooks/useChatAgent";
import { useTravelPlan } from "@/features/travel/hooks/useTravelPlan";
import { useFlightSearch } from "@/features/travel/hooks/useFlightSearch";
import { useSessionStore } from '@/app/store/sessionStore';
import { useTravelPlanStore } from '@/features/travel/store/travelPlanStore';
import { getTravelPlan } from '@/features/travel/services/travelPlanService';
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
    useFlightSearch(client);

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

    const [siderOpen, setSiderOpen] = useState(true);

    return (
        <div className="chat-page">
            <div className="chat-body">
                <div className="chat-main">
                    <TravelPlan />
                </div>
                <div className={`chat-sider-right ${siderOpen ? 'open' : 'collapsed'}`}>
                    <button className="sider-toggle-right" onClick={() => setSiderOpen(o => !o)}>
                        {siderOpen ? <MenuFoldOutlined /> : <MessageOutlined />}
                    </button>
                    {siderOpen && (
                        <div className="sider-exchanges">
                            {exchanges.length === 0 ? (
                                <Welcome />
                            ) : (
                                <Exchanges exchanges={exchanges} />
                            )}
                        </div>
                    )}
                </div>
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
    )
}

export default ChatPage;
