import "./chat.css";

interface UserMessageProps {
    content: string;
}

const UserMessage = ({ content }: UserMessageProps) => {
    return (
        <div className="user-message">
            {content}
        </div>
    );
};

export default UserMessage;
