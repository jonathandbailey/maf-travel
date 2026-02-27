import { Card, Input } from "antd";

interface ChatInputProps {
    value: string;
    onChange: (value: string) => void;
    onKeyDown: (e: React.KeyboardEvent<HTMLInputElement>) => void;
}

const ChatInput = ({ value, onChange, onKeyDown }: ChatInputProps) => {
    return (
        <Card
            style={{
                width: "100%",
                maxWidth: 768,
                minWidth: 700,
                marginBottom: 24,
                marginTop: 0,
                margin: "0 auto 24px",
                border: "1px solid #d9d9d9",

                borderRadius: 16,
            }}
        >
            <Input
                placeholder="Ask me anything..."
                variant="borderless"
                value={value}
                onChange={(e) => onChange(e.target.value)}
                onKeyDown={onKeyDown}
                style={{ flex: 1, width: "100%" }}
            />
        </Card>
    );
};

export default ChatInput;
