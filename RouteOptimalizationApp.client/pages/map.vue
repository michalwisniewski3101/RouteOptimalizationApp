<template>
  <v-app>
    <nav class="no-scrollbar">
      <v-navigation-drawer app v-model="drawer" :width="drawerWidth">
        <v-list>
          <v-list-group :value="true" prepend-icon="mdi-file-upload" class="custom-list-item">
            <template v-slot:activator>
              <v-list-item title="Przesyłanie pliku">
                <v-list-item-content>
                  <v-list-item-title>Przesyłanie pliku</v-list-item-title>
                </v-list-item-content>
              </v-list-item>
            </template>
            <v-list-item>
              <v-list-item-content class="file-select">
                <input type="file" ref="fileInput" @change="handleFileUpload" accept=".json" />
              </v-list-item-content>
            </v-list-item>
            <!-- Przycisk wysyłania zapytania -->
            <v-list-item prepend-icon="mdi-send" @click="sendRequest" class="custom-list-item">
              <v-list-item-content>
                <v-list-item-title>Wyślij zapytanie</v-list-item-title>
              </v-list-item-content>
            </v-list-item>
          </v-list-group>
        </v-list>
        <!-- Sekcja z metrykami -->
        <v-list expand>
          <v-list-group v-if="metrics" prepend-icon="mdi-chart-bar" no-action class="custom-list-group">
            <template v-slot:activator>
              <v-list-item title="Podsumowanie tras">
                <v-list-item-content>
                  <v-list-item-title>Podsumowanie tras</v-list-item-title>
                </v-list-item-content>
              </v-list-item>
            </template>
            <v-list-item-group disabled>
              <v-list-item>
                <v-list-item-content>
                  <v-list-item-title>Liczba tras</v-list-item-title>
                  <v-list-item-subtitle>{{ metrics.usedVehicleCount }}</v-list-item-subtitle>
                </v-list-item-content>
              </v-list-item>
              <v-list-item>
                <v-list-item-content>
                  <v-list-item-title>Całkowita liczba przesyłek</v-list-item-title>
                  <v-list-item-subtitle>{{ metrics.aggregatedRouteMetrics.performedShipmentCount }}</v-list-item-subtitle>
                </v-list-item-content>
              </v-list-item>
              <v-list-item>
                <v-list-item-content>
                  <v-list-item-title>Całkowity czas podróży</v-list-item-title>
                  <v-list-item-subtitle>{{ formatDuration(metrics.aggregatedRouteMetrics.travelDuration) }}</v-list-item-subtitle>
                </v-list-item-content>
              </v-list-item>
              <v-list-item>
                <v-list-item-content>
                  <v-list-item-title>Całkowita odległość</v-list-item-title>
                  <v-list-item-subtitle>{{ metrics.aggregatedRouteMetrics.travelDistanceMeters / 1000 }} km</v-list-item-subtitle>
                </v-list-item-content>
              </v-list-item>
              <v-list-item>
                <v-list-item-content>
                  <v-list-item-title>Całkowity koszt</v-list-item-title>
                  <v-list-item-subtitle>{{ metrics.totalCost }} PLN</v-list-item-subtitle>
                </v-list-item-content>
              </v-list-item>
            </v-list-item-group>
          </v-list-group>
        </v-list>
        <!-- Sekcja z trasami -->
        <v-list expand>
          <v-list-group v-if="routes.length > 0" prepend-icon="mdi-road" no-action class="custom-list-group">
            <template v-slot:activator>
              <v-list-item title="Informacje o trasach">
                <v-list-item-content>
                  <v-list-item-title>Informacje o trasach</v-list-item-title>
                </v-list-item-content>
              </v-list-item>
            </template>
            <v-list-item-group>
              <v-list-group v-for="(route, index) in routes" :key="index" prepend-icon="mdi-truck"
                class="route-info-group" :disabled="route.routeTotalCost == 0">
                <template v-slot:activator>
                  <v-list-item :title="'Pojazd ' + (index + 1)" :disabled="route.routeTotalCost == 0">
                    <v-list-item-content>
                      <v-list-item-title>Pojazd {{ index + 1 }}</v-list-item-title>
                    </v-list-item-content>
                  </v-list-item>
                </template>
                <v-list-item-group >

                  <v-list-item>
                    <v-list-item-content>
                      <v-list-item-title>Koszt trasy:</v-list-item-title>
                      <v-list-item-subtitle>{{ route.routeTotalCost }} PLN</v-list-item-subtitle>
                    </v-list-item-content>
                  </v-list-item>

                  <v-list-item @click="centerMapOnPoint(route.start.arrivalLocation)">
                    <v-list-item-content>
                      <v-list-item-title>Start:</v-list-item-title>
                      <v-list-item-subtitle>{{ formatTime(route.start.vehicleStartTime) }}</v-list-item-subtitle>
                    </v-list-item-content>
                  </v-list-item>

                  <v-list-item v-for="(visit, visitIndex) in route.visits" :key="visitIndex"
                    @click="centerMapOnPoint(visit.arrivalLocation)">
                    <v-list-item-content>
                      <v-list-item-title>Punkt {{ visitIndex + 1 }}:</v-list-item-title>
                      <v-list-item-subtitle>{{ formatTime(visit.startTime) }}</v-list-item-subtitle>
                    </v-list-item-content>
                  </v-list-item>


                  <v-list-item @click="centerMapOnPoint(route.end.arrivalLocation)">
                    <v-list-item-content>
                      <v-list-item-title>Koniec:</v-list-item-title>
                      <v-list-item-subtitle>{{ formatTime(route.end.vehicleEndTime) }}</v-list-item-subtitle>
                    </v-list-item-content>
                  </v-list-item>

                </v-list-item-group>
              </v-list-group>
            </v-list-item-group>
          </v-list-group>
        </v-list>
        <div class="resize-handle" @mousedown="initResize"></div>
      </v-navigation-drawer>
    </nav>
    <v-main class="no-scroll">
      <v-btn absolute bottom left color="primary" @click="drawer = !drawer" style="z-index: 1; margin: 16px;">
        <v-icon>
          {{ drawer ? drawerCloseIcon : drawerOpenIcon }}
        </v-icon>
      </v-btn>
      <GmapMap ref="map" :center="center" :zoom="zoom"
        style="width: 100%; height: 100vh; position: relative; z-index: 0;">
        <GmapMarker v-for="(location, index) in locations" :key="index" :position="location" />
        <GmapPolyline v-for="(route, index) in routePaths" :key="index" :path="route.path" :options="{
          strokeColor: route.color,
          strokeOpacity: 1.5,
          strokeWeight: 3,
        }" />
      </GmapMap>
    </v-main>
  </v-app>
</template>

<script setup>
import { ref } from 'vue'

const center = ref({ lat: 52.114339, lng: 19.423672 })
const zoom = ref(6.2)
const locations = ref([])
const routePaths = ref([])
const routes = ref([])
const metrics = ref(null)
const colors = ['#FF0000', '#0000FF', '#00FF00', '#FFFF00', '#FF00FF']
let requestBody = null
const drawer = ref(true)
const drawerOpenIcon = 'mdi-chevron-right'
const drawerCloseIcon = 'mdi-chevron-left'
const drawerWidth = ref(256)

// Initialization
onMounted(() => {
  initMap()
})

function initMap() {
  if (!window.google) return

  const directionsService = new google.maps.DirectionsService()
  const directionsRenderer = new google.maps.DirectionsRenderer()
  directionsRenderer.setMap(map.value.$mapObject)
}

function initResize(e) {
  const startX = e.clientX
  const startWidth = drawerWidth.value

  const onMouseMove = (moveEvent) => {
    const newWidth = startWidth + (moveEvent.clientX - startX)
    if (newWidth >= 200 && newWidth <= 600) {
      drawerWidth.value = newWidth
    }
  }

  const onMouseUp = () => {
    document.removeEventListener('mousemove', onMouseMove)
    document.removeEventListener('mouseup', onMouseUp)
  }

  document.addEventListener('mousemove', onMouseMove)
  document.addEventListener('mouseup', onMouseUp)
}

function centerMapOnPoint(point) {
  if (point) {
    center.value = {
      lat: point.latitude,
      lng: point.longitude,
    }
    zoom.value = 13
  }
}

function handleFileUpload(event) {
  const file = event.target.files[0]
  const reader = new FileReader()

  reader.onload = function (e) {
    const contents = e.target.result
    requestBody = JSON.parse(contents)
  }

  reader.readAsText(file)
}

async function sendRequest() {
  if (!requestBody) {
    alert('Proszę najpierw przesłać plik JSON.')
    return
  }

  try {
    const { data } = await useFetch('https://planer.joptimizer.com.pl/pl/api/v1/vrp/solution', {
      method: 'POST',
      body: requestBody,
      headers: {
        'Content-Type': 'application/json',
      },
    })

    const solution = data.value
    if (!solution) {
      alert('Brak rozwiązania VRP')
      return
    }

    // Obsługa metryk
    metrics.value = solution.metrics

    routes.value = solution.routes

    const paths = []
    solution.routes.forEach((route, routeIndex) => {
      const path = []
      path.push({
        lat: route.start.arrivalLocation.latitude,
        lng: route.start.arrivalLocation.longitude,
      })

      route.visits.forEach((visit) => {
        path.push({
          lat: visit.arrivalLocation.latitude,
          lng: visit.arrivalLocation.longitude,
        })
      })

      path.push({
        lat: route.end.arrivalLocation.latitude,
        lng: route.end.arrivalLocation.longitude,
      })

      paths.push({
        path,
        color: colors[routeIndex % colors.length],
      })
    })

    routePaths.value = paths

    // Dodanie punktów początkowych, końcowych oraz odwiedzin
    const newLocations = []
    solution.routes.forEach((route) => {
      newLocations.push({
        lat: route.start.arrivalLocation.latitude,
        lng: route.start.arrivalLocation.longitude,
      })

      route.visits.forEach((visit) => {
        newLocations.push({
          lat: visit.arrivalLocation.latitude,
          lng: visit.arrivalLocation.longitude,
        })
      })

      newLocations.push({
        lat: route.end.arrivalLocation.latitude,
        lng: route.end.arrivalLocation.longitude,
      })
    })

    locations.value = newLocations
  } catch (error) {
    console.error('Error sending request:', error)
  }
}

function formatTime(time) {
  return new Date(time).toLocaleTimeString()
}

function formatDuration(duration) {
  const hours = Math.floor(duration / 3600)
  const minutes = Math.floor((duration % 3600) / 60)
  return `${hours} godz. ${minutes} min.`
}
</script>

<style scoped>
.v-main {
  padding: 0;
}

.no-scroll {
  overflow: hidden;
}

.no-scrollbar::-webkit-scrollbar {
  display: none;
}

.file-select {
  margin: 20px 0;
}

.resize-handle {
  width: 5px;
  cursor: ew-resize;
  background-color: rgba(0, 0, 0, 0.1);
  height: 100%;
  position: absolute;
  top: 0;
  right: 0;
}
</style>
