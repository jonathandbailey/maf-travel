import UserMessage from "./UserMessage";
import AssistantMessage from "./AssistantMessage";
import Error from "./Error";
import Status from "./Status";

interface ExchangeProps {
    userContent: string;
    assistantContent?: string;
    error?: string;
}

const Exchange = ({ userContent, assistantContent, error }: ExchangeProps) => {
    return (
        <div style={{ display: "flex", flexDirection: "column", gap: 64, marginBottom: 32 }}>
            <UserMessage content={userContent} />
            {!assistantContent && !error && <Status />}
            {assistantContent && <AssistantMessage content={assistantContent} />}
            {error && <Error message={error} />}
        </div>
    );
};

export default Exchange;
