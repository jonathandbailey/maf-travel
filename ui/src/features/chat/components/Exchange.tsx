import AssistantMessage from "./AssistantMessage";
import ChatError from "./ChatError";
import ThoughtProcess from "./ThoughtProcess";
import type { StatusUpdate } from "../domain/StatusUpdate";
import UserMessage from "./UserMessage";
import Status from "./Status";
import "./chat.css";

interface ExchangeProps {
    userContent: string;
    assistantContent?: string;
    statusUpdates: StatusUpdate[];
    error?: string;
}

const Exchange = ({ userContent, assistantContent, statusUpdates, error }: ExchangeProps) => {
    const isLoading = !assistantContent && !error;

    return (
        <div role="article" className="exchange">
            <UserMessage content={userContent} />
            {isLoading && statusUpdates.length === 0 && <Status />}
            {statusUpdates.length > 0 && (
                <ThoughtProcess statusUpdates={statusUpdates} isLoading={isLoading} />
            )}
            {assistantContent && <AssistantMessage content={assistantContent} />}
            {error && <ChatError message={error} />}
        </div>
    );
};

export default Exchange;
