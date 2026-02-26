import Markdown from "react-markdown";

interface AssistantMessageProps {
    content: string;
}

const AssistantMessage = ({ content }: AssistantMessageProps) => {
    return (
        <div style={{ alignSelf: "flex-start", background: "#f5f5f5", padding: "8px 12px", borderRadius: 8, maxWidth: "80%" }}>
            <Markdown>{content}</Markdown>
        </div>
    );
};

export default AssistantMessage;
