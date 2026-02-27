interface UserMessageProps {
    content: string;
}

const UserMessage = ({ content }: UserMessageProps) => {
    return (
        <div style={{
            alignSelf: "flex-end",
            background: "#F0EEE6", padding: "8px 12px", borderRadius: 8, maxWidth: "80%", fontSize: "1.05rem"
        }}>
            {content}
        </div>
    );
};

export default UserMessage;
