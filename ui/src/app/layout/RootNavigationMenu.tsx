import { Menu } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { useTravelPlanStore } from '../../features/travel/store/travelPlanStore';
import './layout.css';

const RootNavigationMenu = () => {
    const createPlan = useTravelPlanStore((s) => s.createPlan);

    return (
        <Menu
            mode="inline"
            defaultSelectedKeys={['chat']}
            className="nav-menu"
            items={[

                {
                    key: 'new-travel-plan',
                    label: 'New Travel Plan',
                    icon: <PlusOutlined />,
                    onClick: createPlan,
                },
            ]}
        />
    );
};

export default RootNavigationMenu;
