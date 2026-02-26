import { Spin } from "antd";

interface StatusProps {
    message?: string;
}

const Status = ({ message }: StatusProps) => {
    return (
        <div style={{ alignSelf: "flex-start", padding: "8px 12px", display: "flex", alignItems: "center", gap: 8 }}>
            <Spin />
            {message && <span style={{ color: "#888", fontSize: "0.95rem" }}>{message}</span>}
        </div>
    )
}

export default Status;