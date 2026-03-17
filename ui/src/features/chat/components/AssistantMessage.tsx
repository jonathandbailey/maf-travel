import Markdown from "react-markdown";
import "./chat.css";

interface AssistantMessageProps {
    content: string;
}

const AssistantMessage = ({ content }: AssistantMessageProps) => {
    return (
        <div className="assistant-message">
            <Markdown>{content}</Markdown>
        </div>
    );
};

export default AssistantMessage;
