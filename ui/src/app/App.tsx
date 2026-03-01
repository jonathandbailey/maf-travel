import './App.css'
import { Layout } from 'antd';
import ChatPage from '../pages/ChatPage';
import RootHeader from './layout/RootHeader';
import RootNavigationMenu from './layout/RootNavigationMenu';

const { Header, Content, Sider } = Layout;

function App() {

  return (
    <>
      <Layout className="app-layout">
        <Header className="app-header">
          <RootHeader />
        </Header>
        <Layout className="app-body">
          <Sider width={250} className="app-sider"><RootNavigationMenu /></Sider>
          <Content className="app-content">
            <ChatPage />
          </Content>
        </Layout>
      </Layout>
    </>
  )
}

export default App
