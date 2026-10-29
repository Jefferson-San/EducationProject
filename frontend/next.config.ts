import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  cacheComponents: true,
  partialPrefetching: true,
  // Gera .next/standalone com um server.js mínimo, usado pela imagem Docker.
  output: "standalone",
};

export default nextConfig;
