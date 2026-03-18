import { Card, List, Typography } from "antd";
import type { FlightOptionResponse, FlightSearchResponse } from "../services/flightSearchService";
import "./travel.css";

const { Text } = Typography;

const formatDateTime = (value: string) => {
    const d = new Date(value);
    return d.toLocaleString(undefined, { dateStyle: "short", timeStyle: "short" });
};

const formatPrice = (value: number) =>
    new Intl.NumberFormat(undefined, { style: "currency", currency: "USD" }).format(value);

const FlightOptionItem = ({ option }: { option: FlightOptionResponse }) => (
    <List.Item>
        <div style={{ width: "100%" }}>
            <div className="flight-option-header">
                <Text strong>{option.flightNumber}</Text>
                <Text>{option.airline}</Text>
            </div>
            <div className="flight-option-route">
                <Text type="secondary">{option.origin} → {option.destination}</Text>
            </div>
            <div className="flight-option-times">
                <Text type="secondary" className="flight-option-times-text">
                    {formatDateTime(option.departureTime)} – {formatDateTime(option.arrivalTime)}
                </Text>
            </div>
            <div className="flight-option-footer">
                <Text>{formatPrice(option.pricePerPerson)} / person</Text>
                <Text type="secondary">{option.availableSeats} seats</Text>
            </div>
        </div>
    </List.Item>
);

const FlightSearch = ({ flightSearch }: { flightSearch: FlightSearchResponse }) => {
    return (
        <Card title="Flight Options" size="small">
            <List
                size="small"
                dataSource={flightSearch.flightOptions}
                renderItem={(option) => <FlightOptionItem key={option.id} option={option} />}
            />
        </Card>
    );
};

export default FlightSearch;
