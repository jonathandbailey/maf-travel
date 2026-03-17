import { useEffect, useRef } from "react";
import Exchange from "./Exchange";
import type { ExchangeItem } from "../hooks/useChatAgent";

interface ExchangesProps {
    exchanges: ExchangeItem[];
}

const Exchanges = ({ exchanges }: ExchangesProps) => {
    const bottomRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        bottomRef.current?.scrollIntoView({ behavior: "smooth" });
    }, [exchanges]);

    return (
        <div className="chat-messages-inner" role="log" aria-live="polite" aria-label="Conversation">
            {exchanges.map((ex) => (
                <Exchange key={ex.id} userContent={ex.userContent} assistantContent={ex.assistantContent} statusUpdates={ex.statusUpdates} error={ex.error} />
            ))}
            <div ref={bottomRef} />
        </div>
    );
};

export default Exchanges;
