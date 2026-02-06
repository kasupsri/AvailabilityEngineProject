import "../index.css";
import { ThemeProvider } from "@/components/ui/theme-provider";
import { ThemeToggle } from "@/components/ui/theme-toggle";
import { Link, Outlet, useLocation } from "react-router-dom";
import { HelmetProvider } from "react-helmet-async";
import { SidebarProvider, Sidebar, SidebarContent, SidebarHeader, SidebarMenu, SidebarMenuItem, SidebarMenuButton, SidebarTrigger } from "@/components/ui/sidebar";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Calendar, Search, GalleryVerticalEnd } from "lucide-react";
import { Toaster } from "react-hot-toast";

const version = "1.0.0";

const navItems = [
  { title: "Set busy", href: "/", icon: Calendar },
  { title: "Find availability", href: "/find", icon: Search },
];

export default function RootLayout() {
  const location = useLocation();

  return (
    <HelmetProvider>
      <ThemeProvider defaultTheme="system">
        <SidebarProvider>
          <Sidebar variant="sidebar" collapsible="offcanvas">
            <SidebarHeader>
              <SidebarMenu>
                <SidebarMenuItem>
                  <SidebarMenuButton size="lg" asChild>
                    <Link to="/">
                      <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
                        <GalleryVerticalEnd className="size-4" />
                      </div>
                      <div className="flex flex-col gap-0.5 leading-none">
                        <span className="font-semibold">Calendar Availability</span>
                        <Badge variant="secondary" className="w-fit">v{version}</Badge>
                      </div>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              </SidebarMenu>
            </SidebarHeader>
            <SidebarContent>
              <div className="flex flex-col h-full">
                <div className="space-y-4 py-4 flex-1">
                  <div className="px-3 py-2">
                    <div className="space-y-1">
                      {navItems.map((item) => (
                        <Button
                          key={item.href}
                          variant={location.pathname === item.href ? "secondary" : "ghost"}
                          className="w-full justify-start"
                          asChild
                        >
                          <Link to={item.href}>
                            <item.icon className="mr-2 h-4 w-4" />
                            {item.title}
                          </Link>
                        </Button>
                      ))}
                    </div>
                  </div>
                </div>
              </div>
            </SidebarContent>
          </Sidebar>
          <div className="flex-1 flex flex-col min-w-0 bg-background border-l border-border/60">
            <header className="flex items-center justify-between gap-2 px-4 py-3 sm:px-6 sm:py-4 border-b border-border bg-background shrink-0">
              <SidebarTrigger />
              <div className="flex-1 min-w-0" />
              <ThemeToggle />
            </header>
            <main className="flex-1 overflow-auto min-h-0">
              <div className="w-full max-w-6xl xl:max-w-7xl px-4 py-4 sm:px-6 sm:py-6">
                <Outlet />
              </div>
            </main>
          </div>
        </SidebarProvider>
        <Toaster position="bottom-right" />
      </ThemeProvider>
    </HelmetProvider>
  );
}
