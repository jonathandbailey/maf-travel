import { Card, Descriptions } from "antd";
import { useTravelPlanStore } from "../store/travelPlanStore";

const formatDate = (value: string | null | undefined) => {
    if (!value) return "—";
    const d = new Date(value);
    return `${d.getDate()}.${d.getMonth() + 1}.${d.getFullYear()}`;
};

const TravelPlan = () => {
    const plan = useTravelPlanStore((s) => s.plan);

    const hasValues = plan && Object.values(plan).some((v) => v !== null);
    if (!hasValues) return null;

    return (
        <Card title="Travel Plan" size="small" style={{ border: "1px solid #d9d9d9", }}>
            <Descriptions column={1} size="small">
                <Descriptions.Item label="Origin">{plan?.origin ?? "—"}</Descriptions.Item>
                <Descriptions.Item label="Destination">{plan?.destination ?? "—"}</Descriptions.Item>
                <Descriptions.Item label="Departure Date">{formatDate(plan?.startDate)}</Descriptions.Item>
                <Descriptions.Item label="Return Date">{formatDate(plan?.endDate)}</Descriptions.Item>
                <Descriptions.Item label="Travelers">{plan?.numberOfTravelers ?? "—"}</Descriptions.Item>
            </Descriptions>
        </Card>
    );
};

export default TravelPlan;
