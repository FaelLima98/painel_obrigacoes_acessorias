import { useState, type ReactNode } from 'react'
import { Layout, Menu, Typography, type MenuProps } from 'antd'
import {
  BankOutlined,
  BellOutlined,
  CalendarOutlined,
  DashboardOutlined,
} from '@ant-design/icons'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import { colors } from '../theme/colors'

const { Header, Sider, Content } = Layout
const { Title } = Typography

interface NavItem {
  key: string
  label: string
  icon: ReactNode
}

const navItems: NavItem[] = [
  { key: '/', label: 'Dashboard', icon: <DashboardOutlined /> },
  { key: '/empresas', label: 'Empresas', icon: <BankOutlined /> },
  { key: '/calendario', label: 'Calendário', icon: <CalendarOutlined /> },
  { key: '/alertas', label: 'Alertas', icon: <BellOutlined /> },
]

const SIDER_WIDTH = 200
const COLLAPSED_WIDTH = 80

const pageTitles: Record<string, string> = {
  '/': 'Dashboard',
  '/empresas': 'Empresas',
  '/calendario': 'Calendário de Obrigações',
  '/alertas': 'Painel de Alertas',
}

export function AppLayout() {
  const [collapsed, setCollapsed] = useState(false)
  const location = useLocation()
  const navigate = useNavigate()

  const selectedKey =
    navItems.find((i) => i.key !== '/' && location.pathname.startsWith(i.key))?.key ?? '/'
  const title = pageTitles[selectedKey] ?? 'e-Auditoria'

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        collapsible
        collapsed={collapsed}
        onCollapse={setCollapsed}
        breakpoint="lg"
        width={SIDER_WIDTH}
        collapsedWidth={COLLAPSED_WIDTH}
        style={{
          background: colors.dark,
          position: 'fixed',
          insetInlineStart: 0,
          top: 0,
          bottom: 0,
          height: '100vh',
          overflow: 'auto',
        }}
      >
        <div
          style={{
            height: 56,
            margin: 16,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#fff',
            fontWeight: 700,
            fontSize: collapsed ? 16 : 20,
            letterSpacing: 0.5,
            whiteSpace: 'nowrap',
          }}
        >
          {collapsed ? 'eA' : 'e-Auditoria'}
        </div>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[selectedKey]}
          onClick={({ key }) => navigate(key)}
          items={navItems as MenuProps['items']}
          style={{ background: 'transparent' }}
        />
      </Sider>

      <Layout
        style={{
          marginInlineStart: collapsed ? COLLAPSED_WIDTH : SIDER_WIDTH,
          transition: 'margin-inline-start 0.2s',
        }}
      >
        <Header
          style={{
            background: '#fff',
            padding: '0 24px',
            display: 'flex',
            alignItems: 'center',
            boxShadow: '0 1px 4px rgba(0,0,0,0.08)',
          }}
        >
          <Title level={4} style={{ margin: 0, color: colors.dark }}>
            {title}
          </Title>
        </Header>

        <Content style={{ margin: 24, background: colors.surface }}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  )
}
