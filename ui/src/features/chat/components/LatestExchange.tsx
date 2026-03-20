import UserMessage from "./UserMessage";
import Status from "./Status";
import AssistantMessage from "./AssistantMessage";
import type { ExchangeItem } from "../hooks/useChatAgent";
import "./chat.css";

interface LatestExchangeProps {
    exchange: ExchangeItem | undefined;
}

const LatestExchange = ({ exchange }: LatestExchangeProps) => {
    if (!exchange) return null;

    const isLoading = !exchange.assistantContent && !exchange.error;
    const latestStatus = exchange.statusUpdates.at(-1)?.status;

    return (
        <div className="latest-exchange">
            <UserMessage content={exchange.userContent} />
            {isLoading && <Status message={latestStatus} />}
            {exchange.assistantContent && <AssistantMessage content={exchange.assistantContent} />}
        </div>
    );
};

export default LatestExchange;
