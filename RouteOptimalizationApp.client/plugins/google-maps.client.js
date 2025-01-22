import { defineNuxtPlugin } from '#app'
import VueGoogleMaps from '@fawmi/vue-google-maps'

export default defineNuxtPlugin(nuxtApp => {
  nuxtApp.vueApp.use(VueGoogleMaps, {
    load: {
      key: 'AIzaSyDBYCFmIcpWHuuOEsxLECsoky3bFhvCMN8',
      libraries: 'places,geometry'
    },
  })
})
