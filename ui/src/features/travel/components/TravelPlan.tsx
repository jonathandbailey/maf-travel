import { Card, Flex, Typography, theme } from "antd";
import {
    CalendarOutlined,
    EnvironmentOutlined,
    TeamOutlined,
} from "@ant-design/icons";
import { useTravelPlanStore } from "../store/travelPlanStore";

const { Text } = Typography;

const formatDate = (value: string | null | undefined) => {
    if (!value) return null;
    const d = new Date(value);
    return `${d.getDate()}.${d.getMonth() + 1}.${d.getFullYear()}`;
};

const StatCard = ({
    icon,
    label,
    value,
    subIcon,
    subLabel,
}: {
    icon: React.ReactNode;
    label: string;
    value: string | number | null | undefined;
    subIcon?: React.ReactNode;
    subLabel?: string | null;
}) => {
    const { token } = theme.useToken();
    const display = value != null ? String(value) : null;
    return (
        <Card style={{ flex: 1, minWidth: 160 }}>
            <Flex vertical gap={4}>
                <Text type="secondary" style={{ fontSize: 13 }}>
                    <span style={{ marginRight: 6 }}>{icon}</span>
                    {label}
                </Text>
                <Text style={{ fontSize: 22, fontWeight: 600, color: token.colorText }}>
                    {display ?? "—"}
                </Text>
                {subLabel !== undefined && (
                    <Text type="secondary" style={{ fontSize: 13 }}>
                        <span style={{ marginRight: 6 }}>{subIcon}</span>
                        {subLabel ?? "—"}
                    </Text>
                )}
            </Flex>
        </Card>
    );
};

const TravelPlan = () => {
    const plan = useTravelPlanStore((s) => s.plan);

    const hasValues = plan && Object.values(plan).some((v) => v !== null);
    if (!hasValues) return null;

    return (
        <Flex justify="center" style={{ paddingTop: 24 }}>
            <Flex gap={16} wrap="wrap" style={{ width: "100%", maxWidth: 720 }}>
                <StatCard
                    icon={<EnvironmentOutlined />}
                    label="Origin"
                    value={plan?.origin}
                    subIcon={<CalendarOutlined />}
                    subLabel={formatDate(plan?.startDate)}
                />
                <StatCard
                    icon={<EnvironmentOutlined />}
                    label="Destination"
                    value={plan?.destination}
                    subIcon={<CalendarOutlined />}
                    subLabel={formatDate(plan?.endDate)}
                />
                <StatCard
                    icon={<TeamOutlined />}
                    label="Travelers"
                    value={plan?.numberOfTravelers}
                />
            </Flex>
        </Flex>
    );
};

export default TravelPlan;
