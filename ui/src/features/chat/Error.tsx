interface ErrorProps {
    message: string;
}

const Error = ({ message }: ErrorProps) => {
    return (
        <div style={{ alignSelf: "flex-start", background: "#fff2f0", border: "1px solid #ffccc7", color: "#cf1322", padding: "8px 12px", borderRadius: 8, maxWidth: "80%" }}>
            {message}
        </div>
    );
};

export default Error;
