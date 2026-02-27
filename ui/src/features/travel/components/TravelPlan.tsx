import { Card, Descriptions } from "antd";
import type { TravelPlan as TravelPlanModel } from "../domain/TravelPlan";

interface TravelPlanProps {
    travelPlan: TravelPlanModel;
}

const TravelPlan = ({ travelPlan }: TravelPlanProps) => {
    return (
        <Card title="Travel Plan" size="small">
            <Descriptions column={1} size="small">
                <Descriptions.Item label="Origin">{travelPlan.origin ?? "—"}</Descriptions.Item>
                <Descriptions.Item label="Destination">{travelPlan.destination ?? "—"}</Descriptions.Item>
                <Descriptions.Item label="Departure Date">{travelPlan.startDate ?? "—"}</Descriptions.Item>
                <Descriptions.Item label="Return Date">{travelPlan.endDate ?? "—"}</Descriptions.Item>
                <Descriptions.Item label="Travelers">{travelPlan.numberOfTravelers ?? "—"}</Descriptions.Item>
            </Descriptions>
        </Card>
    );
};

export default TravelPlan;
