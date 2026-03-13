import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Alert, Button, Card, Col, Row, Spin, Typography } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import {
    createTravelPlan,
    listTravelPlans,
    type TravelPlanResponse,
} from '../features/travel/services/travelPlanService';
import './DashboardPage.css';

const { Title, Text } = Typography;

const formatDate = (date: string | null) =>
    date ? new Date(date).toLocaleDateString() : null;

const DashboardPage = () => {
    const navigate = useNavigate();
    const [plans, setPlans] = useState<TravelPlanResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [creating, setCreating] = useState(false);

    useEffect(() => {
        listTravelPlans()
            .then(setPlans)
            .catch((e: Error) => setError(e.message))
            .finally(() => setLoading(false));
    }, []);

    const handleCreate = async () => {
        setCreating(true);
        try {
            const plan = await createTravelPlan();
            navigate(`/travel-plans/${plan.id}`);
        } catch (e) {
            setError((e as Error).message);
            setCreating(false);
        }
    };

    const handleCardClick = (id: string) => {
        navigate(`/travel-plans/${id}`);
    };

    return (
        <div className="dashboard-page">
            <div className="dashboard-header">
                <Title level={3} style={{ margin: 0 }}>Travel Plans</Title>
                <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    onClick={handleCreate}
                    loading={creating}
                >
                    New Travel Plan
                </Button>
            </div>

            {error && (
                <Alert
                    type="error"
                    message={error}
                    closable
                    onClose={() => setError(null)}
                    style={{ marginBottom: 16 }}
                />
            )}

            {loading ? (
                <div className="dashboard-loading">
                    <Spin size="large" />
                </div>
            ) : plans.length === 0 ? (
                <div className="dashboard-empty">
                    <Text type="secondary">No travel plans yet. Create one to get started.</Text>
                </div>
            ) : (
                <Row gutter={[16, 16]}>
                    {plans.map((plan) => (
                        <Col key={plan.id} xs={24} sm={12} lg={8} xl={6}>
                            <Card
                                hoverable
                                onClick={() => handleCardClick(plan.id)}
                                title={plan.destination ?? 'No destination set'}
                            >
                                {plan.origin && (
                                    <p><Text type="secondary">From: </Text>{plan.origin}</p>
                                )}
                                {(plan.startDate || plan.endDate) && (
                                    <p>
                                        <Text type="secondary">Dates: </Text>
                                        {formatDate(plan.startDate)} – {formatDate(plan.endDate)}
                                    </p>
                                )}
                                {plan.numberOfTravelers !== null && (
                                    <p>
                                        <Text type="secondary">Travelers: </Text>
                                        {plan.numberOfTravelers}
                                    </p>
                                )}
                            </Card>
                        </Col>
                    ))}
                </Row>
            )}
        </div>
    );
};

export default DashboardPage;
