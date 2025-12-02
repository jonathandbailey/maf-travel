import ChatInput from "./ChatInput"

const RootLayout = () => {
    return <div><ChatInput onEnter={(value) => console.log(value)} /></div>
}

export default RootLayout