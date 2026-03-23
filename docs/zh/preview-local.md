---
title: 本地预览说明
description: GitHub Pages 文档站的本地预览方式，以及 Ruby、Bundler 和 Jekyll 的基本准备步骤。
lang: zh-CN
---

# 本地预览说明

当前文档站基于 GitHub Pages 兼容的 Jekyll 结构组织。如果你想在本地预览样式、导航和页面布局，推荐先准备 Ruby 与 Bundler 环境。

## 需要准备

- Ruby
- Bundler
- 在仓库根目录执行 `bundle install`

## 本地预览步骤

1. 安装 Ruby。
2. 安装 Bundler：`gem install bundler`
3. 在仓库根目录执行：`bundle install`
4. 按仓库现有脚本或标准 Jekyll 方式启动本地预览。
5. 如果采用标准方式，可执行：`bundle exec jekyll serve --source docs`

## 预览时重点检查

- `zh/` 与 `en/` 两套目录是否都能正常访问
- 首页、导航页、博客索引页的卡片链接是否正确
- 文章页代码块、表格、引用块样式是否正常
- 头部导航和页脚链接是否都指向当前语言目录

## 常见问题

### 找不到 `bundle`

通常说明 Bundler 没有安装，先执行：

```bash
gem install bundler
```

### 找不到 `jekyll`

如果仓库使用 Bundler 管理依赖，请优先使用：

```bash
bundle exec jekyll serve --source docs
```

### 页面样式没更新

可以尝试停止本地服务后重新启动，或清理 `.jekyll-cache` 后再次预览。