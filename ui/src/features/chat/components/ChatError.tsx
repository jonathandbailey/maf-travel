import "./chat.css";

interface ChatErrorProps {
    message: string;
}

const ChatError = ({ message }: ChatErrorProps) => {
    return (
        <div className="chat-error">
            {message}
        </div>
    );
};

export default ChatError;
