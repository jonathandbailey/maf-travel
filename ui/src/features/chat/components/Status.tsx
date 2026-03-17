import { Spin } from "antd";
import "./chat.css";

interface StatusProps {
    message?: string;
}

const Status = ({ message }: StatusProps) => {
    return (
        <div className="status" aria-live="polite" aria-label="Loading response">
            <Spin />
            {message && <span className="status-message">{message}</span>}
        </div>
    )
}

export default Status;
