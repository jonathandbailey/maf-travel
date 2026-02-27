import Exchange from "./Exchange";
import type { ExchangeItem } from "../hooks/useChatAgent";

interface ExchangesProps {
    exchanges: ExchangeItem[];
}

const Exchanges = ({ exchanges }: ExchangesProps) => {
    return (
        <div className="chat-messages-inner">
            {exchanges.map((ex) => (
                <Exchange key={ex.id} userContent={ex.userContent} assistantContent={ex.assistantContent} statusUpdates={ex.statusUpdates} error={ex.error} />
            ))}
        </div>
    );
};

export default Exchanges;
