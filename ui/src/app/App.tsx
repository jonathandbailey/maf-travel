import './App.css'
import { Layout } from 'antd';
import ChatPage from '../pages/ChatPage';
import RootHeader from './layout/RootHeader';
import RootNavigationMenu from './layout/RootNavigationMenu';
import ErrorBoundary from './ErrorBoundary';

const { Header, Content, Sider } = Layout;

function App() {

  return (
    <ErrorBoundary>
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
    </ErrorBoundary>
  )
}

export default App
