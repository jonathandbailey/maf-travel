import AssistantMessage from "./AssistantMessage";
import Error from "./Error";

import ThoughtProcess from "./ThoughProcess";
import type { StatusUpdate } from "../domain/StatusUpdate";
import UserMessage from "./UserMessage";
import Status from "./Status";

interface ExchangeProps {
    userContent: string;
    assistantContent?: string;
    statusUpdates: StatusUpdate[];
    error?: string;
}

const Exchange = ({ userContent, assistantContent, statusUpdates, error }: ExchangeProps) => {
    const isLoading = !assistantContent && !error;

    return (
        <div style={{ display: "flex", flexDirection: "column", gap: 64, marginBottom: 32 }}>
            <UserMessage content={userContent} />
            {isLoading && statusUpdates.length === 0 && <Status />}
            {statusUpdates.length > 0 && (
                <ThoughtProcess statusUpdates={statusUpdates} isLoading={isLoading} />
            )}
            {assistantContent && <AssistantMessage content={assistantContent} />}
            {error && <Error message={error} />}
        </div>
    );
};

export default Exchange;
