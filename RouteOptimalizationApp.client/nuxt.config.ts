import { defineNuxtConfig } from 'nuxt/config'

export default defineNuxtConfig({
  ssr: false,

  app: {
    head: {
      titleTemplate: '%s - test001',
      title: 'test001',
      htmlAttrs: {
        lang: 'en'
      },
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { hid: 'description', name: 'description', content: '' },

        { name: 'format-detection', content: 'telephone=no' }
      ],
      link: [
        { rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' }
      ]
    }
  },

  css: [],

  plugins: [
    { src: '~/plugins/google-maps.client.js', mode: 'client' }  // W Nuxt 3 używamy 'mode' zamiast 'ssr'
  ],

  components: true,

  modules: [
  ],

  vite: {
    define: {
      'process.env.DEBUG': false
    },
    optimizeDeps: {
      include: ['fast-deep-equal']
    }
  },

  build: {
    transpile: ['vuetify']
  },

  devtools: {
    enabled: true
  },

  compatibilityDate: '2025-01-22'
})