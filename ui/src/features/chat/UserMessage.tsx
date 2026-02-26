interface UserMessageProps {
    content: string;
}

const UserMessage = ({ content }: UserMessageProps) => {
    return (
        <div style={{ alignSelf: "flex-end", background: "#e6f7ff", padding: "8px 12px", borderRadius: 8, maxWidth: "80%" }}>
            {content}
        </div>
    );
};

export default UserMessage;
