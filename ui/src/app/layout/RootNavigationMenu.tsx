import { Menu } from 'antd';
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
                    onClick: createPlan,
                },
            ]}
        />
    );
};

export default RootNavigationMenu;
