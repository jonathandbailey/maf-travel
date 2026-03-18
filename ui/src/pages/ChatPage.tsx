import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import ChatInput from "@/features/chat/components/ChatInput";
import LatestExchange from "@/features/chat/components/LatestExchange";
import TravelPlan from "@/features/travel/components/TravelPlan";
import Welcome from "@/features/travel/components/Welcome";
import FlightPlan from "@/features/travel/components/FlightPlan";
import { useChatAgent } from "@/features/chat/hooks/useChatAgent";
import { useChatStore } from "@/features/chat/store/chatStore";
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
    const plan = useTravelPlanStore((s) => s.plan);
    const hasValues = plan && Object.values(plan).some((v) => v !== null);

    useEffect(() => {
        createPlan();
        getTravelPlan(id!).then(({ origin, destination, startDate, endDate, numberOfTravelers, sessionId }) => {
            updatePlan({ origin, destination, startDate, endDate, numberOfTravelers });
            if (sessionId) setSessionId(sessionId);
        });
    }, [id, setSessionId, createPlan, updatePlan]);

    const [inputValue, setInputValue] = useState("");
    const { streamingExchange, isStreaming, sendMessage, handleCancel, client } = useChatAgent();
    useTravelPlan(client);
    useFlightSearch(client);

    const exchanges = useChatStore((s) => s.exchanges);
    const lastCompleted = exchanges[exchanges.length - 1];
    const displayExchange = streamingExchange ?? lastCompleted ?? undefined;

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
                {hasValues ? <TravelPlan /> : <Welcome />}
                <FlightPlan />
            </div>
            <LatestExchange exchange={displayExchange} />
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
