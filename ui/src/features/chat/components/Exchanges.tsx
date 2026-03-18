import { useEffect, useRef } from "react";
import Exchange from "./Exchange";
import type { ExchangeItem } from "../hooks/useChatAgent";

interface ExchangesProps {
    exchanges: ExchangeItem[];
}

const Exchanges = ({ exchanges }: ExchangesProps) => {
    const containerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (containerRef.current) {
            containerRef.current.scrollTop = containerRef.current.scrollHeight;
        }
    }, [exchanges]);

    return (
        <div ref={containerRef} className="chat-messages-inner" role="log" aria-live="polite" aria-label="Conversation">
            {exchanges.map((ex) => (
                <Exchange key={ex.id} userContent={ex.userContent} assistantContent={ex.assistantContent} statusUpdates={ex.statusUpdates} error={ex.error} />
            ))}
        </div>
    );
};

export default Exchanges;
