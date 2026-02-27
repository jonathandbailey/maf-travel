import { Flex, Typography } from "antd";

const Welcome = () => {
    return (
        <Flex justify="center" align="center" style={{ height: "100%" }}>
            <Typography.Title level={2}>Where would you like to go today?</Typography.Title>
        </Flex>
    );
};

export default Welcome;
