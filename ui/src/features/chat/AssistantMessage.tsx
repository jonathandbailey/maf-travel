import Markdown from "react-markdown";

interface AssistantMessageProps {
    content: string;
}

const AssistantMessage = ({ content }: AssistantMessageProps) => {
    return (
        <div style={{ alignSelf: "flex-start", padding: "8px 12px", borderRadius: 8, maxWidth: "80%", fontFamily: "'Roboto', sans-serif", fontSize: "1.05rem" }}>
            <Markdown>{content}</Markdown>
        </div>
    );
};

export default AssistantMessage;
