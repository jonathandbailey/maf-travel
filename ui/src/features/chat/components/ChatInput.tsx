import { Card, Input, Dropdown, Button } from "antd";
import { PlusOutlined, ArrowUpOutlined, StopOutlined } from "@ant-design/icons";
import type { MenuProps } from "antd";
import "./chat.css";

interface ChatInputProps {
    value: string;
    onChange: (value: string) => void;
    onKeyDown: (e: React.KeyboardEvent<HTMLInputElement>) => void;
    onSuggestionSelect?: (suggestion: string) => void;
    onSubmit?: () => void;
    onCancel?: () => void;
    isStreaming?: boolean;
}

const suggestions: MenuProps["items"] = [
    {
        key: "capabilities",
        label: "Show me the application capabilities",
    },
    {
        key: "plan_paris_trip",
        label: "Plan a trip to Paris on the 01.06.2026 for 4 people",
    },
];

const ChatInput = ({ value, onChange, onKeyDown, onSuggestionSelect, onSubmit, onCancel, isStreaming }: ChatInputProps) => {
    const handleMenuClick: MenuProps["onClick"] = ({ key }) => {
        const item = suggestions?.find((s) => s?.key === key);
        if (item && "label" in item && typeof item.label === "string") {
            onSuggestionSelect?.(item.label);
        }
    };

    return (
        <Card className="chat-input-card">
            <div className="chat-input-row">
                <Dropdown
                    menu={{ items: suggestions, onClick: handleMenuClick }}
                    placement="topLeft"
                    trigger={["click"]}
                >
                    <Button type="text" icon={<PlusOutlined />} aria-label="Choose a suggestion" />
                </Dropdown>
                <Input
                    placeholder="Ask me anything..."
                    variant="borderless"
                    value={value}
                    onChange={(e) => onChange(e.target.value)}
                    onKeyDown={onKeyDown}
                    style={{ flex: 1 }}
                    aria-label="Message input"
                />
                <Button
                    type="primary"
                    shape="circle"
                    icon={isStreaming ? <StopOutlined /> : <ArrowUpOutlined />}
                    onClick={isStreaming ? onCancel : onSubmit}
                    disabled={!isStreaming && !value.trim()}
                    aria-label={isStreaming ? "Stop streaming" : "Send message"}
                />
            </div>
        </Card>
    );
};

export default ChatInput;
