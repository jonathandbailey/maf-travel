import './App.css'
import { Layout } from 'antd';
import ChatPage from '../pages/ChatPage';
import RootHeader from './layout/RootHeader';
import RootNavigationMenu from './layout/RootNavigationMenu';

const { Header, Content, Sider } = Layout;

function App() {

  return (
    <>
      <Layout style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
        <Header style={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end', padding: '0 16px', background: 'white', borderBottom: '1px solid #e0e0e0', flexShrink: 0 }}>
          <RootHeader />
        </Header>
        <Layout style={{ flex: 1, minHeight: 0, overflow: 'hidden', background: '#FAF9F5' }}>
          <Sider width={250} style={{ minHeight: 0, borderRight: '1px solid #d9d9d9', background: '#FAF9F5' }}><RootNavigationMenu /></Sider>
          <Content style={{ height: '100%', minHeight: 0, overflow: 'hidden', padding: '16px' }}>
            <ChatPage />
          </Content>
        </Layout>
      </Layout>
    </>
  )
}

export default App
