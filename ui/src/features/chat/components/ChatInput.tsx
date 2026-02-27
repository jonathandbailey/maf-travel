import { Card, Input, Dropdown, Button } from "antd";
import { PlusOutlined, ArrowUpOutlined, StopOutlined } from "@ant-design/icons";
import type { MenuProps } from "antd";

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
        label: "Plan a trip to Paris for next month for 4 people",
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
            <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                <Dropdown
                    menu={{ items: suggestions, onClick: handleMenuClick }}
                    placement="topLeft"
                    trigger={["click"]}
                >
                    <Button type="text" icon={<PlusOutlined />} />
                </Dropdown>
                <Input
                    placeholder="Ask me anything..."
                    variant="borderless"
                    value={value}
                    onChange={(e) => onChange(e.target.value)}
                    onKeyDown={onKeyDown}
                    style={{ flex: 1 }}
                />
                <Button
                    type="primary"
                    shape="circle"
                    icon={isStreaming ? <StopOutlined /> : <ArrowUpOutlined />}
                    onClick={isStreaming ? onCancel : onSubmit}
                    disabled={!isStreaming && !value.trim()}
                />
            </div>
        </Card>
    );
};

export default ChatInput;
