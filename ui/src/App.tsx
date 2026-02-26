import './App.css'
import { Layout } from 'antd';
import ChatPage from './pages/ChatPage';

const { Header, Content } = Layout;

function App() {

  return (
    <>
      <Layout style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
        <Header style={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end', padding: '0 16px', background: 'white', borderBottom: '1px solid #e0e0e0', flexShrink: 0 }}>

        </Header>
        <Layout style={{ flex: 1, minHeight: 0, overflow: 'hidden', background: 'white' }}>
          <Content style={{ height: '100%', minHeight: 0, overflow: 'hidden', padding: '16px' }}>
            <ChatPage />
          </Content>
        </Layout>
      </Layout>
    </>
  )
}

export default App
