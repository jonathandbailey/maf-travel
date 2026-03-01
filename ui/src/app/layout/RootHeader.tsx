import { Typography } from "antd";
import TravelIcon from '../../assets/fly.png';
import './layout.css';

const { Title } = Typography;

const RootHeader = () => {
    return (
        <div className="header-container">
            <img src={TravelIcon} alt="Travel App Logo" className="header-logo" />
            <Title level={4} className="header-title">Travel</Title>
        </div>
    );
}

export default RootHeader;